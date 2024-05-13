import { Line } from '@react-three/drei'
import { Frame } from '../shared/types/frame'
import { Signal } from '../shared/types/signal'
import { useProjectStore } from '../store/projectStore'
import useViewerStore from '../store/viewerStore'

export default function CollectedDataPath() {
    const { selectedSignal, scale } = useViewerStore()

    const { project, simulationDeltaIndex, simulationState } = useProjectStore()

    if (!project) {
        throw new Error('Project не задан')
    }

    if (!project.Result) {
        return <></>
    }

    if (!selectedSignal) {
        return <></>
    }

    const selectedFrame = project.Result.AllFrames[selectedSignal.FrameId]
    const data = selectedFrame.CollectedData

    if (!data) {
        return <></>
    }

    // находим все фреймы, в которых передавалась
    // текущая информация с датчиков
    const framesWithData: [number, Frame][] = []

    for (let i = 0; i < project.Result.AllFrames.length; i++) {
        const frame = project.Result.AllFrames[i]

        if (
            frame.CollectedData?.SensorId === data.SensorId &&
            frame.CollectedData?.CycleId === data.CycleId
        ) {
            framesWithData.push([i, frame])
        }
    }

    // находим все сигналы, в которых передавались
    // фреймы с текущей информацией с датчиков
    const signalsWithData: [number, Signal, boolean][] = []

    for (const frameWithData of framesWithData) {
        for (let i = 0; i < project.Result.AllSignals.length; i++) {
            const signal = project.Result.AllSignals[i]

            if (signal.FrameId === frameWithData[0]) {
                signalsWithData.push([i, signal, false])
            }
        }
    }

    console.log(signalsWithData)

    // определяем, был ли сигнал "в прошлом" или "в будущем"
    for (const signalWithData of signalsWithData) {
        for (let i = 0; i < project.Result.Deltas.length; i++) {
            const delta = project.Result.Deltas[i]

            if (!delta.SignalDeltas) {
                continue
            }

            if (delta.SignalDeltas.length > 0) {
                for (const signal of delta.SignalDeltas) {
                    if (signal.SignalId === signalWithData[0]) {
                        signalWithData[2] = i > simulationDeltaIndex
                    }
                }
            }
        }
    }

    // ищем путь от сенсора до референса поиском в глубину
    const fromSensor = simulationState.Sensors[data.SensorId]
    const traverse: (id: number) => number[] | undefined = (id: number) => {
        const sensor = simulationState.Sensors[id]
        if (sensor.ClusterId === fromSensor.ClusterId && sensor.IsReference) {
            return [id]
        }

        const receivers = signalsWithData
            .filter((signal) => signal[1].SenderId === id)
            .map((signal) => signal[1].ReceiverId)

        if (receivers.length === 0) {
            return undefined
        }

        for (const receiver of receivers) {
            const result = traverse(receiver)
            if (result !== undefined) {
                return [sensor.Id, ...result]
            }
        }

        return undefined
    }

    const path = traverse(fromSensor.Id)

    if (!path) {
        console.log('Не удалось найти путь до референса')
        return <></>
    }

    console.log(path)

    const signalsPathToReference: [number, Signal, boolean][] = []
    for (let i = 0; i < path.length; i++) {
        if (i === 0) continue

        const signal = signalsWithData.filter(
            (s) => s[1].SenderId === path[i - 1] && s[1].ReceiverId === path[i]
        )

        signalsPathToReference.push(signal[0])
    }

    const vertexColorsPast: [number, number, number][] = [[0.8, 0.1, 0.1]]
    const vertexColorsFuture: [number, number, number][] = [[0.8, 0.5, 0.5]]

    const lines: React.ReactNode[] = []

    let i = 0
    for (const signalWithData of signalsPathToReference) {
        const sender = project.Environment.Sensors[signalWithData[1].SenderId]
        const receiver =
            project.Environment.Sensors[signalWithData[1].ReceiverId]

        const from = {
            X: sender.Position.X / scale,
            Y: sender.Position.Y / scale,
            Z: sender.Position.Z / scale
        }

        const to = {
            X: receiver.Position.X / scale,
            Y: receiver.Position.Y / scale,
            Z: receiver.Position.Z / scale
        }

        lines.push(
            <Line
                key={i}
                points={[
                    [from.X, from.Y, from.Z],
                    [to.X, to.Y, to.Z]
                ]}
                vertexColors={
                    signalWithData[2] ? vertexColorsPast : vertexColorsFuture
                }
                dashed={true}
                lineWidth={6}
            />
        )
        i++
    }

    return <>{lines}</>
}
