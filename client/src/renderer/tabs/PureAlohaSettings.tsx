import { Grid, Input, Text } from '@chakra-ui/react'
import { UseFormReturn } from 'react-hook-form'
import { SensorSettings } from '../shared/types/sensorSettings'

type Props = {
    form: UseFormReturn<SensorSettings>
}

export default function PureAlohaSettings({ form }: Props) {
    return (
        <>
            <Grid
                templateColumns='min-content auto'
                gap={4}
                alignItems='center'
            >
                <Text whiteSpace='nowrap'>Время ожидания (сек):</Text>
                <Input
                    {...form.register('DataLinkProtocol.Timeout', {
                        required: true,
                        min: 0
                    })}
                    fontWeight={
                        form.formState.dirtyFields.DataLinkProtocol?.Timeout
                            ? 'bold'
                            : undefined
                    }
                />

                <Text>Отклонение времени ожидания:</Text>
                <Input
                    {...form.register(
                        'DataLinkProtocol.TimeoutRelativeDeviation',
                        { required: true, min: 0, max: 1 }
                    )}
                    fontWeight={
                        form.formState.dirtyFields.DataLinkProtocol
                            ?.TimeoutRelativeDeviation
                            ? 'bold'
                            : undefined
                    }
                />

                <Text whiteSpace='nowrap'>Время ожидания ACK:</Text>
                <Input
                    {...form.register('DataLinkProtocol.AckTimeout', {
                        required: true,
                        min: 0
                    })}
                    fontWeight={
                        form.formState.dirtyFields.DataLinkProtocol?.AckTimeout
                            ? 'bold'
                            : undefined
                    }
                />

                <Text whiteSpace='nowrap'>
                    Количество попыток ожидания ACK:
                </Text>
                <Input
                    {...form.register('DataLinkProtocol.AckRetries', {
                        required: true,
                        min: 0
                    })}
                    fontWeight={
                        form.formState.dirtyFields.DataLinkProtocol?.AckRetries
                            ? 'bold'
                            : undefined
                    }
                />
            </Grid>
        </>
    )
}
