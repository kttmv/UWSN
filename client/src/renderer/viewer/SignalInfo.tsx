import { Card, Flex, GridItem, Text } from '@chakra-ui/react'
import { Signal } from '../shared/types/signal'
import { useProjectStore } from '../store/projectStore'

type Props = {
    signal: Signal
}

export default function SignalInfo({ signal }: Props) {
    const { project } = useProjectStore()

    if (!project) {
        throw new Error('Project не определен')
    }

    if (!project.Result) {
        throw new Error('Результаты симуляции не определены')
    }

    const frame = project.Result.AllFrames[signal.FrameId]

    const sensorFrom = project.Environment.Sensors[signal.SenderId]
    const sensorTo = project.Environment.Sensors[signal.ReceiverId]

    const positionFromString =
        `{ X: ${sensorFrom.Position.X.toFixed(1)}, ` +
        `Y: ${sensorFrom.Position.Y.toFixed(1)}, ` +
        `Z: ${sensorFrom.Position.Z.toFixed(1)} }`

    const positionToString =
        `{ X: ${sensorTo.Position.X.toFixed(1)}, ` +
        `Y: ${sensorTo.Position.Y.toFixed(1)}, ` +
        `Z: ${sensorTo.Position.Z.toFixed(1)} }`

    const distance = Math.sqrt(
        Math.pow(sensorFrom.Position.X - sensorTo.Position.X, 2) +
            Math.pow(sensorFrom.Position.Y - sensorTo.Position.Y, 2) +
            Math.pow(sensorFrom.Position.Z - sensorTo.Position.Z, 2)
    )

    return (
        <>
            <GridItem colSpan={2}>
                <Flex direction='row' gap={4} alignItems='center'>
                    <Text whiteSpace='nowrap'>От:</Text>
                    <Card flexGrow={1} padding='5px'>
                        #{sensorFrom.Id}
                    </Card>
                    <Text whiteSpace='nowrap'>До:</Text>
                    <Card flexGrow={1} padding='5px'>
                        #{sensorTo.Id}
                    </Card>
                </Flex>
            </GridItem>

            <Text whiteSpace='nowrap'>Координаты от:</Text>
            <Card padding='5px'>{positionFromString}</Card>

            <Text whiteSpace='nowrap'>Координаты до:</Text>
            <Card padding='5px'>{positionToString}</Card>

            <Text whiteSpace='nowrap'>Расстояние:</Text>
            <Card padding='5px'>{distance.toFixed(2)} м</Card>
        </>
    )
}
