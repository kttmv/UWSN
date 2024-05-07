import { Card, Text } from '@chakra-ui/react'
import { Sensor } from '../shared/types/sensor'
import { useProjectStore } from '../store/projectStore'

type Props = {
    sensor: Sensor
}

export default function SensorInfo({ sensor }: Props) {
    const { project } = useProjectStore()
    if (!project) {
        throw new Error('Project не определен')
    }

    const positionString =
        `{ X: ${sensor.Position.X.toFixed(1)}, ` +
        `Y: ${sensor.Position.Y.toFixed(1)}, ` +
        `Z: ${sensor.Position.Z.toFixed(1)} }`

    let clusterString =
        sensor.ClusterId > -1 ? sensor.ClusterId.toString() : 'Отсутствует'

    if (sensor.IsReference) {
        clusterString += ', является референсным'
    }

    return (
        <>
            <Text whiteSpace='nowrap'>Координаты:</Text>
            <Card padding='5px'>{positionString}</Card>

            <Text whiteSpace='nowrap'>Кластер:</Text>
            <Card padding='5px'>{clusterString}</Card>

            <Text whiteSpace='nowrap'>Батарея:</Text>
            <Card padding='5px'>
                {sensor.Battery.toFixed(2)} (
                {(
                    (sensor.Battery /
                        project.SensorSettings.InitialSensorBattery) *
                    100
                ).toFixed(2)}
                %)
            </Card>
        </>
    )
}
