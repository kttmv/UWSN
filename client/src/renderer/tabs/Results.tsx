import { Flex, Text } from '@chakra-ui/react'
import { useProjectStore } from '../store/projectStore'
import TotalEnergy from './TotalEnergy'

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
            <TotalEnergy />
        </Flex>
    )
}
