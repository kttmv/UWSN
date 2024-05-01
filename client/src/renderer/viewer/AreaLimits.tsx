import { Vector3 } from '../shared/types/vector3'
import GridRectangle from './GridRectangle'

interface Props {
    v1: Vector3
    v2: Vector3
}

export default function AreaLimits({ v1, v2 }: Props) {
    return (
        <>
            <GridRectangle
                v1={v1}
                v2={{
                    X: v2.X + 0.5,
                    Y: v2.Y + 0.5,
                    Z: v1.Z - 0.5
                }}
            />

            <GridRectangle
                v1={v1}
                v2={{
                    X: v2.X + 0.5,
                    Y: v1.Y - 0.5,
                    Z: v2.Z + 0.5
                }}
            />

            <GridRectangle
                v1={v1}
                v2={{
                    X: v1.X - 0.5,
                    Y: v2.Y + 0.5,
                    Z: v1.Z - 0.5
                }}
            />

            <GridRectangle
                v1={v2}
                v2={{
                    X: v2.X + 0.5,
                    Y: v1.Y - 0.5,
                    Z: v1.Z - 0.5
                }}
            />

            <GridRectangle
                v1={v2}
                v2={{
                    X: v1.X - 0.5,
                    Y: v2.Y + 0.5,
                    Z: v1.Z - 0.5
                }}
            />

            <GridRectangle
                v1={v2}
                v2={{
                    X: v1.X - 0.5,
                    Y: v1.Y - 0.5,
                    Z: v2.Z + 0.5
                }}
            />
        </>
    )
}
