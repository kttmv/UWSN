import { FormLabel, Select } from '@chakra-ui/react'
import { UseFormReturn } from 'react-hook-form'
import {
    isMultichanneledAloha,
    isPureAloha
} from '../shared/types/dataLinkProtocol'
import { DataLinkProtocolType } from '../shared/types/dataLinkProtocolType'
import { SensorSettings } from '../shared/types/sensorSettings'
import MultiChanneledAlohaSettings from './MultiChanneledAlohaSettings'
import PureAlohaSettings from './PureAlohaSettings'

type Props = {
    form: UseFormReturn<SensorSettings>
}

export default function SensorProtocolSettings({ form }: Props) {
    return (
        <>
            <FormLabel>Канальный протокол</FormLabel>
            <Select
                {...form.register('DataLinkProtocol.$type', {
                    required: true
                })}
                fontWeight={
                    form.formState.dirtyFields.DataLinkProtocol?.$type
                        ? 'bold'
                        : undefined
                }
            >
                <option value={DataLinkProtocolType.PureAloha}>
                    Pure ALOHA (одноканальный)
                </option>
                <option value={DataLinkProtocolType.MultiChanneledAloha}>
                    Pure ALOHA (многоканальный)
                </option>
            </Select>

            {isPureAloha(form.watch().DataLinkProtocol) && (
                <PureAlohaSettings form={form} />
            )}

            {isMultichanneledAloha(form.watch().DataLinkProtocol) && (
                <MultiChanneledAlohaSettings form={form} />
            )}
        </>
    )
}
