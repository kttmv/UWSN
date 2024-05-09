import { Vector3 } from '../shared/types/vector3'
import GridRectangle from './GridRectangle'

interface Props {
    v1: Vector3
    v2: Vector3
}

export default function AreaLimits({ v1, v2 }: Props) {
    const padding = 1

    const newV1 = {
        X: v1.X - padding,
        Y: v1.Y - padding,
        Z: v1.Z - padding
    }

    const newV2 = {
        X: v2.X + padding,
        Y: v2.Y + padding,
        Z: v2.Z + padding
    }

    return (
        <>
            <GridRectangle
                v1={newV1}
                v2={{
                    X: newV2.X,
                    Y: newV2.Y,
                    Z: newV1.Z
                }}
            />

            <GridRectangle
                v1={newV1}
                v2={{
                    X: newV2.X,
                    Y: newV1.Y,
                    Z: newV2.Z
                }}
            />

            <GridRectangle
                v1={newV1}
                v2={{
                    X: newV1.X,
                    Y: newV2.Y,
                    Z: newV1.Z
                }}
            />

            <GridRectangle
                v1={newV2}
                v2={{
                    X: newV2.X,
                    Y: newV1.Y,
                    Z: newV1.Z
                }}
            />

            <GridRectangle
                v1={newV2}
                v2={{
                    X: newV1.X,
                    Y: newV2.Y,
                    Z: newV1.Z
                }}
            />

            <GridRectangle
                v1={newV2}
                v2={{
                    X: newV1.X,
                    Y: newV1.Y,
                    Z: newV2.Z
                }}
            />
        </>
    )
}
