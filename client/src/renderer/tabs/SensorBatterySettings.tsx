import { FormLabel, Grid, Input, Text } from '@chakra-ui/react'
import { UseFormReturn } from 'react-hook-form'
import { SensorSettings } from '../shared/types/sensorSettings'

type Props = {
    form: UseFormReturn<SensorSettings>
}

export default function SensorBatterySettings({ form }: Props) {
    return (
        <>
            <FormLabel marginTop={10}>Параметры батареи</FormLabel>
            <Grid
                alignItems='center'
                templateColumns='min-content auto'
                gap={4}
            >
                <Text whiteSpace='nowrap'>Начальный заряд батареи, Дж:</Text>
                <Input
                    type='number'
                    {...form.register('InitialSensorBattery')}
                    fontWeight={
                        form.formState.dirtyFields.InitialSensorBattery
                            ? 'bold'
                            : undefined
                    }
                />
                <Text whiteSpace='nowrap'>Считать умершим при заряде, Дж:</Text>
                <Input
                    type='number'
                    {...form.register('BatteryDeadCharge')}
                    fontWeight={
                        form.formState.dirtyFields.BatteryDeadCharge
                            ? 'bold'
                            : undefined
                    }
                />
            </Grid>
        </>
    )
}
