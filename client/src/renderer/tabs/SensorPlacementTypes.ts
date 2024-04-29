export const enum SensorPlacementId {
    RandomStep
}
export type SensorPlacementType = {
    id: SensorPlacementId
    name: string
    verb: string
}

export const SENSOR_PLACEMENT_TYPES: SensorPlacementType[] = [
    {
        id: SensorPlacementId.RandomStep,
        name: 'Ортогональное распределение со случайным шагом',
        verb: 'placeSensorsRndStep'
    }
]
