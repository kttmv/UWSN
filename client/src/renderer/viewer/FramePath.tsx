import { Line } from '@react-three/drei'
import { Frame } from '../shared/types/frame'
import { Signal } from '../shared/types/signal'
import { useProjectStore } from '../store/projectStore'
import useViewerStore from '../store/viewerStore'
import CollectedDataPath from './CollectedDataPath'

export default function FramePath() {
    const { selectedSignal, scale } = useViewerStore()

    const { project, simulationDeltaIndex } = useProjectStore()

    if (!project) {
        throw new Error('Project не задан')
    }

    if (!project.Result) {
        return <></>
    }

    if (!selectedSignal) {
        return <></>
    }

    const selectedFrame: [number, Frame] = [
        selectedSignal.FrameId,
        project.Result.AllFrames[selectedSignal.FrameId]
    ]

    // находим все сигналы, в которых передавался данный фрейм
    const signalsWithData: [number, Signal, boolean][] = []

    for (let i = 0; i < project.Result.AllSignals.length; i++) {
        const signal = project.Result.AllSignals[i]

        if (signal.FrameId == selectedFrame[0]) {
            signalsWithData.push([i, signal, false])
        }
    }

    // определяем, был ли сигнал "в прошлом" или "в будущем"
    for (const signalWithData of signalsWithData) {
        for (let i = 0; i < project.Result.Deltas.length; i++) {
            const delta = project.Result.Deltas[i]

            if (delta.SignalDeltas.length > 0) {
                for (const signal of delta.SignalDeltas) {
                    if (signal.SignalId == signalWithData[0]) {
                        signalWithData[2] = i > simulationDeltaIndex
                    }
                }
            }
        }
    }

    const vertexColorsPast: [number, number, number][] = [[0.05, 0.05, 0.05]]
    const vertexColorsFuture: [number, number, number][] = [[0.25, 0.25, 0.25]]

    const lines: React.ReactNode[] = []

    let i = 0
    for (const signalWithData of signalsWithData) {
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
                lineWidth={2}
            />
        )
        i++
    }

    return (
        <>
            <>{lines}</>
            <CollectedDataPath />
        </>
    )
}
