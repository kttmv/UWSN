import { FormLabel, Grid, Input, Text } from '@chakra-ui/react'
import { UseFormReturn } from 'react-hook-form'
import { SensorSettingsInputs } from './SensorSettings'

type Props = {
    form: UseFormReturn<SensorSettingsInputs>
}

export default function SensorBatterySettings({ form }: Props) {
    return (
        <>
            <FormLabel marginTop={10}>Параметры батареи:</FormLabel>
            <Grid alignItems='center' templateColumns='min-content auto'>
                <Text whiteSpace='nowrap'>Начальный заряд энергии:</Text>
                <Input
                    type='number'
                    {...form.register('InitialSensorBattery')}
                />
            </Grid>
        </>
    )
}
