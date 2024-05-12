import { Grid, Input, Text } from '@chakra-ui/react'
import { UseFormReturn } from 'react-hook-form'
import { SensorSettings } from '../shared/types/sensorSettings'

type Props = {
    form: UseFormReturn<SensorSettings>
}

export default function RetardedClusterizationSettings({ form }: Props) {
    return (
        <Grid templateColumns='min-content auto' gap={4}>
            <Text whiteSpace='nowrap'>Количество кластеров по X:</Text>
            <Input
                isInvalid={
                    form.formState.errors.ClusterizationAlgorithm
                        ?.XClusterCount !== undefined
                }
                type='number'
                flexGrow={1}
                {...form.register('ClusterizationAlgorithm.XClusterCount', {
                    required: true
                })}
                fontWeight={
                    form.formState.dirtyFields.ClusterizationAlgorithm
                        ?.XClusterCount
                        ? 'bold'
                        : undefined
                }
            />

            <Text whiteSpace='nowrap'>Количество кластеров по Z:</Text>
            <Input
                isInvalid={
                    form.formState.errors.ClusterizationAlgorithm
                        ?.ZClusterCount !== undefined
                }
                type='number'
                flexGrow={1}
                {...form.register('ClusterizationAlgorithm.ZClusterCount', {
                    required: true
                })}
                fontWeight={
                    form.formState.dirtyFields.ClusterizationAlgorithm
                        ?.ZClusterCount
                        ? 'bold'
                        : undefined
                }
            />
        </Grid>
    )
}
