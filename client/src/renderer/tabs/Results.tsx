import { Flex, Text } from '@chakra-ui/react'
import { useProjectStore } from '../store/projectStore'
import EnergyPerSensor from './EnergyPerSensor'
import TotalEnergy from './TotalEnergy'
import TotalEnergyOfSelectedSensor from './TotalEnergyOfSelectedSensor'

export default function Results() {
    const { project } = useProjectStore()

    if (!project) {
        throw new Error('Project не определен')
    }

    if (!project.Result) {
        return <Text>Запустите симуляцию, чтобы увидеть ее результаты</Text>
    }

    return (
        <Flex direction='column' gap={4}>
            <TotalEnergyOfSelectedSensor />
            <EnergyPerSensor />
            <TotalEnergy />
        </Flex>
    )
}
