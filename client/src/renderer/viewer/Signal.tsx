import { Line } from '@react-three/drei'
import { useState } from 'react'
import { FrameType } from '../shared/types/frame'
import { Signal } from '../shared/types/signal'
import { useProjectStore } from '../store/projectStore'
import useViewerStore from '../store/viewerStore'

type Props = {
    signal: Signal
}

export default function Signal({ signal }: Props) {
    const { selectedSignal, setSelectedSignal, scale } = useViewerStore()

    const { project } = useProjectStore()

    if (!project) {
        throw new Error('Project не задан')
    }

    if (!project.Result) {
        throw new Error('Результаты симуляции не определены')
    }

    const [isHovered, setIsHovered] = useState(false)

    const isSelected =
        selectedSignal && selectedSignal.FrameId === signal.FrameId

    const setSelected = () => {
        if (!isSelected) setSelectedSignal(signal)
        else setSelectedSignal(undefined)
    }

    const frame = project.Result.AllFrames[signal.FrameId]

    let vertexColors: [number, number, number][]

    if (isSelected) {
        vertexColors = [
            [1.0, 1.0, 1.0],
            [0.3, 0.6, 0.3]
        ]
    } else if (isHovered) {
        vertexColors = [
            [0.2, 0.2, 0.2],
            [0.3, 0.4, 0.3]
        ]
    } else {
        switch (frame.Type) {
            case FrameType.Ack: {
                vertexColors = [
                    [0.0, 1.0, 0.0],
                    [0.0, 0.0, 0.0]
                ]
                break
            }
            case FrameType.Data: {
                vertexColors = [
                    [1.0, 1.0, 0.0],
                    [0.0, 0.0, 0.0]
                ]
                break
            }
            case FrameType.Hello: {
                vertexColors = [
                    [0.0, 0.0, 1.0],
                    [0.0, 0.0, 0.0]
                ]
                break
            }
            case FrameType.Warning: {
                vertexColors = [
                    [1.0, 0.0, 0.0],
                    [0.0, 0.0, 0.0]
                ]
                break
            }
            default: {
                throw new Error('Неизвестный тип фрейма')
            }
        }
    }

    const sender = project.Environment.Sensors[signal.SenderId]
    const receiver = project.Environment.Sensors[signal.ReceiverId]

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

    return (
        <>
            <Line
                onClick={setSelected}
                onPointerOver={() => setIsHovered(true)}
                onPointerOut={() => setIsHovered(false)}
                points={[
                    [from.X, from.Y, from.Z],
                    [to.X, to.Y, to.Z]
                ]}
                vertexColors={vertexColors}
                lineWidth={5}
            />
        </>
    )
}
