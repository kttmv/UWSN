import { Line } from '@react-three/drei'
import { useState } from 'react'
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

    const [hovered, setHover] = useState(false)

    const isSelected =
        selectedSignal && selectedSignal.FrameId === signal.FrameId

    const setSelected = () => {
        if (!isSelected) setSelectedSignal(signal)
        else setSelectedSignal(undefined)
    }

    const vertexColors: [number, number, number][] = isSelected
        ? [
              [0.25, 0.35, 0.25],
              [0.3, 0.6, 0.3]
          ]
        : hovered
          ? [
                [0.2, 0.2, 0.2],
                [0.3, 0.4, 0.3]
            ]
          : [
                [0.05, 0.05, 0.05],
                [0.2, 0.3, 0.3]
            ]

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
                onPointerOver={() => setHover(true)}
                onPointerOut={() => setHover(false)}
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
