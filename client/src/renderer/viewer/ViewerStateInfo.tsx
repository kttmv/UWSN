import { Flex, Text } from '@chakra-ui/react'
import { useProjectStore } from '../store/projectStore'

export default function ViewerStateInfo() {
    const { project, simulationState } = useProjectStore()

    if (!project?.Result) {
        return <></>
    }

    return (
        <Flex
            position='absolute'
            bottom={20}
            left={5}
            direction='column'
            gap={1}
        >
            <Text>Время: {simulationState.Time.replace('T', ' ')}</Text>
            <Text>Цикл: {simulationState.CycleId}</Text>
        </Flex>
    )
}
