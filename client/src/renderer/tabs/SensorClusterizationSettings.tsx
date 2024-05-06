import { FormLabel, Select } from '@chakra-ui/react'
import { UseFormReturn } from 'react-hook-form'
import { isRetardedClusterization } from '../shared/types/clsterizationAlgorith'
import { ClusterizationAlgorithmType } from '../shared/types/clusterizationAlogirthmType'
import { SensorSettings } from '../shared/types/sensorSettings'
import RetardedClusterizationSettings from './RetardedClusterizationSettings'

type Props = {
    form: UseFormReturn<SensorSettings>
}

export default function SensorProtocolSettings({ form }: Props) {
    return (
        <>
            <FormLabel marginTop={10}>Алгоритм кластеризации</FormLabel>
            <Select
                {...form.register('ClusterizationAlgorithm.$type', {
                    required: true
                })}
                fontWeight={
                    form.formState.dirtyFields.ClusterizationAlgorithm?.$type
                        ? 'bold'
                        : undefined
                }
            >
                <option
                    value={ClusterizationAlgorithmType.RetardedClusterization}
                >
                    Метод разбиения на равные параллелепипеды
                </option>
            </Select>

            {isRetardedClusterization(form.watch().ClusterizationAlgorithm) && (
                <RetardedClusterizationSettings form={form} />
            )}
        </>
    )
}
