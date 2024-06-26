import { Box } from '@react-three/drei'
import { Vector3 } from '../shared/types/vector3'
import useViewerStore from '../store/viewerStore'

interface Props {
    v1: Vector3
    v2: Vector3
}

export default function GridRectangle({ v1, v2 }: Props) {
    const { scale } = useViewerStore()

    const padding = 0.5

    const v1Scaled = {
        X: v1.X / scale - padding,
        Y: v1.Y / scale - padding,
        Z: v1.Z / scale - padding
    }

    const v2Scaled = {
        X: v2.X / scale + padding,
        Y: v2.Y / scale + padding,
        Z: v2.Z / scale + padding
    }

    const lengthX = Math.abs(v2Scaled.X - v1Scaled.X)
    const lengthY = Math.abs(v2Scaled.Y - v1Scaled.Y)
    const lengthZ = Math.abs(v2Scaled.Z - v1Scaled.Z)

    const startingPosition: Vector3 = {
        X: Math.min(v1Scaled.X, v2Scaled.X) + lengthX / 2,
        Y: Math.min(v1Scaled.Y, v2Scaled.Y) + lengthY / 2,
        Z: Math.min(v1Scaled.Z, v2Scaled.Z) + lengthZ / 2
    }

    const points: [number, number, number][] = []

    points.push([v1Scaled.X, v1Scaled.Y, v1Scaled.Z])
    points.push([v2Scaled.X, v1Scaled.Y, v1Scaled.Z])
    points.push([v2Scaled.X, v1Scaled.Y, v2Scaled.Z])
    points.push([v1Scaled.X, v1Scaled.Y, v2Scaled.Z])
    points.push([v1Scaled.X, v1Scaled.Y, v1Scaled.Z])

    const segmentsX = Math.round(Math.log(lengthX))
    const segmentsY = Math.round(Math.log(lengthY))
    const segmentsZ = Math.round(Math.log(lengthZ))

    return (
        <Box
            args={[lengthX, lengthY, lengthZ, segmentsX, segmentsY, segmentsZ]}
            position={[
                startingPosition.X,
                startingPosition.Y,
                startingPosition.Z
            ]}
        >
            <meshBasicMaterial attach='material' color='blue' wireframe />
        </Box>
    )
}
