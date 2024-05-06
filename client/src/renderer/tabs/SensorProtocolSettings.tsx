import { Flex, FormLabel, Select } from '@chakra-ui/react'
import { UseFormReturn } from 'react-hook-form'
import { isRetardedClusterization } from '../shared/types/clsterizationAlgorith'
import { ClusterizationAlgorithmType } from '../shared/types/clusterizationAlogirthmType'
import { DataLinkProtocolType } from '../shared/types/dataLinkProtocolType'
import { SensorSettings } from '../shared/types/sensorSettings'
import RetardedClusterizationSettings from './RetardedClusterizationSettings'

type Props = {
    form: UseFormReturn<SensorSettings>
}

export default function SensorProtocolSettings({ form }: Props) {
    return (
        <Flex gap={4} direction='column'>
            <FormLabel>Канальный уровень:</FormLabel>
            <Select
                {...form.register('DataLinkProtocol.$type', {
                    required: true
                })}
                fontWeight={
                    form.formState.dirtyFields.DataLinkProtocol?.$type
                        ? 'bold'
                        : undefined
                }
                flexGrow={1}
            >
                <option value={DataLinkProtocolType.PureAloha}>
                    Pure ALOHA (одноканальный)
                </option>
                <option value={DataLinkProtocolType.MultiChanneledAloha}>
                    Pure ALOHA (многоканальный)
                </option>
            </Select>

            <FormLabel>Алгоритм кластеризации:</FormLabel>
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
        </Flex>
    )
}
