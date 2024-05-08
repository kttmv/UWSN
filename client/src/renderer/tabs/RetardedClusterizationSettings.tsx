import { Flex, Input, Text } from '@chakra-ui/react'
import { UseFormReturn } from 'react-hook-form'
import { SensorSettings } from '../shared/types/sensorSettings'

type Props = {
    form: UseFormReturn<SensorSettings>
}

export default function RetardedClusterizationSettings({ form }: Props) {
    return (
        <Flex direction='row' alignItems='center' gap={4}>
            <Text whiteSpace='nowrap'>Количество кластеров:</Text>
            <Input
                type='number'
                flexGrow={1}
                {...form.register('ClusterizationAlgorithm.NumberOfClusters')}
                fontWeight={
                    form.formState.dirtyFields.ClusterizationAlgorithm
                        ?.NumberOfClusters
                        ? 'bold'
                        : undefined
                }
            />
        </Flex>
    )
}
