import { Flex, Input, Text } from '@chakra-ui/react'
import { UseFormRegister } from 'react-hook-form'
import { SensorSettingsInputs } from './SensorSettings'

type Props = {
    register: UseFormRegister<SensorSettingsInputs>
}

export default function RetardedClusterizationSettings({ register }: Props) {
    return (
        <>
            <Flex direction='row' alignItems='center' gap={4}>
                <Text whiteSpace='nowrap'>Количество кластеров:</Text>
                <Input
                    type='number'
                    flexGrow={1}
                    {...register('ClusterizationAlgorithm.NumberOfClusters')}
                />
            </Flex>
        </>
    )
}
