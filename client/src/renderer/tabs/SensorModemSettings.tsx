import { FormLabel, Select } from '@chakra-ui/react'
import { UseFormReturn } from 'react-hook-form'
import { ModemType } from '../shared/types/modem'
import { SensorSettings } from '../shared/types/sensorSettings'

type Props = {
    form: UseFormReturn<SensorSettings>
}

export default function SensorModemSettings({ form }: Props) {
    return (
        <>
            <FormLabel marginTop={10}>Алгоритм кластеризации</FormLabel>
            <Select
                {...form.register('Modem.$type', {
                    required: true
                })}
                fontWeight={
                    form.formState.dirtyFields.Modem?.$type ? 'bold' : undefined
                }
            >
                <option value={ModemType.AquaModem1000}>Aqua Modem 1000</option>

                <option value={ModemType.AquaModem500}>Aqua Modem 500</option>

                <option value={ModemType.AquaCommMako}>AquaComm Mako</option>

                <option value={ModemType.AquaCommMarlin}>
                    AquaComm Marlin
                </option>

                <option value={ModemType.AquaCommOrca}>AquaComm Orca</option>

                <option value={ModemType.MicronModem}>Micron</option>

                <option value={ModemType.SMTUTestModem}>
                    Тестовый модем СПбГМТУ
                </option>
            </Select>
        </>
    )
}