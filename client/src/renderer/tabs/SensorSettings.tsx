import { Button, Flex, FormControl, Text, Tooltip } from '@chakra-ui/react'
import { IconDeviceFloppy } from '@tabler/icons-react'
import { useEffect } from 'react'
import { SubmitHandler, useForm } from 'react-hook-form'
import { SensorSettings } from '../shared/types/sensorSettings'
import { useProjectStore } from '../store/projectStore'
import BatterySettings from './BatterySettings'
import ClusterizationSettings from './ClusterizationSettings'
import DataLinkProtocolSettings from './DataLinkProtocolSettings'
import ModemSettings from './ModemSettings'

export default function SensorSettings() {
    const { project, setProject } = useProjectStore()

    if (!project) {
        throw new Error('Project не определен')
    }

    const defaultValues: SensorSettings = structuredClone(
        project.SensorSettings
    )

    const form = useForm<SensorSettings>({
        defaultValues,
        reValidateMode: 'onBlur',
        mode: 'all'
    })

    // нужно, чтобы при смене проекта выставились правильные значения
    useEffect(() => {
        form.reset(defaultValues)
        console.log(defaultValues)
    }, [project])

    const onSubmit: SubmitHandler<SensorSettings> = async (data) => {
        const newProject = structuredClone(project)
        newProject.SensorSettings = data
        newProject.Result = undefined
        setProject(newProject)
    }

    return (
        <form onSubmit={form.handleSubmit(onSubmit)}>
            <FormControl>
                <Flex direction='column' gap={4}>
                    <DataLinkProtocolSettings form={form} />

                    <ClusterizationSettings form={form} />

                    <ModemSettings form={form} />

                    <BatterySettings form={form} />
                </Flex>
            </FormControl>
            <Tooltip
                isDisabled={project.Result === undefined}
                label='Внимание! Данное действие удалит результаты симуляции'
            >
                <Button
                    marginTop={10}
                    width='100%'
                    isDisabled={
                        !form.formState.isDirty || !form.formState.isValid
                    }
                    type='submit'
                >
                    <IconDeviceFloppy />{' '}
                    <Text marginLeft={1}>
                        Сохранить изменения настройки сенсоров
                    </Text>
                </Button>
            </Tooltip>
        </form>
    )
}
