import { Button, Flex, FormControl, FormLabel, Tooltip } from '@chakra-ui/react'
import { IconDeviceFloppy } from '@tabler/icons-react'
import { useEffect } from 'react'
import { SubmitHandler, useForm } from 'react-hook-form'
import { SensorSettings } from '../shared/types/sensorSettings'
import { useProjectStore } from '../store/projectStore'
import SensorProtocolSettings from './SensorProtocolSettings'

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
            <Flex direction='column' gap={4}>
                <FormControl>
                    <FormLabel>Протоколы</FormLabel>
                    <SensorProtocolSettings form={form} />
                </FormControl>
                <Tooltip
                    isDisabled={project.Result === undefined}
                    label='Внимание! Данное действие удалит результаты симуляции'
                >
                    <Button isDisabled={!form.formState.isDirty} type='submit'>
                        <IconDeviceFloppy />
                        Сохранить изменения настройки сенсоров
                    </Button>
                </Tooltip>
            </Flex>
        </form>
    )
}
