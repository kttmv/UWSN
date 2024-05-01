import { Vector3 } from '../shared/types/vector3'
import GridRectangle from './GridRectangle'

interface Props {
    v1: Vector3
    v2: Vector3
}

export default function AreaLimits({ v1, v2 }: Props) {
    const padding = 1

    return (
        <>
            <GridRectangle
                v1={v1}
                v2={{
                    X: v2.X + padding,
                    Y: v2.Y + padding,
                    Z: v1.Z - padding
                }}
            />

            <GridRectangle
                v1={v1}
                v2={{
                    X: v2.X + padding,
                    Y: v1.Y - padding,
                    Z: v2.Z + padding
                }}
            />

            <GridRectangle
                v1={v1}
                v2={{
                    X: v1.X - padding,
                    Y: v2.Y + padding,
                    Z: v1.Z - padding
                }}
            />

            <GridRectangle
                v1={v2}
                v2={{
                    X: v2.X + padding,
                    Y: v1.Y - padding,
                    Z: v1.Z - padding
                }}
            />

            <GridRectangle
                v1={v2}
                v2={{
                    X: v1.X - padding,
                    Y: v2.Y + padding,
                    Z: v1.Z - padding
                }}
            />

            <GridRectangle
                v1={v2}
                v2={{
                    X: v1.X - padding,
                    Y: v1.Y - padding,
                    Z: v2.Z + padding
                }}
            />
        </>
    )
}
