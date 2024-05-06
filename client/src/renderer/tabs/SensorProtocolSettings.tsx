import { Flex, FormLabel, Select } from '@chakra-ui/react'
import { FormState, UseFormRegister, UseFormWatch } from 'react-hook-form'
import { isRetardedClusterization } from '../shared/types/clsterizationAlgorith'
import { ClusterizationAlgorithmType } from '../shared/types/clusterizationAlogirthmType'
import { DataLinkProtocolType } from '../shared/types/dataLinkProtocolType'
import RetardedClusterizationSettings from './RetardedClusterizationSettings'
import { SensorSettingsInputs } from './SensorSettings'

type Props = {
    register: UseFormRegister<SensorSettingsInputs>
    formState: FormState<SensorSettingsInputs>
    watch: UseFormWatch<SensorSettingsInputs>
}

export default function SensorProtocolSettings({
    register,
    formState,
    watch
}: Props) {
    return (
        <Flex gap={4} direction='column'>
            <FormLabel>Канальный уровень:</FormLabel>
            <Select
                {...register('DataLinkProtocolType', {
                    required: true
                })}
                fontWeight={
                    formState.dirtyFields.DataLinkProtocolType
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
                {...register('ClusterizationAlgorithm.$type', {
                    required: true
                })}
                fontWeight={
                    formState.dirtyFields.ClusterizationAlgorithm?.$type
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

            {isRetardedClusterization(watch().ClusterizationAlgorithm) && (
                <RetardedClusterizationSettings register={register} />
            )}
        </Flex>
    )
}
