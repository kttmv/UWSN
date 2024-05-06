import { Button, Flex, FormControl, FormLabel, Tooltip } from '@chakra-ui/react'
import { IconDeviceFloppy } from '@tabler/icons-react'
import { useEffect } from 'react'
import { SubmitHandler, useForm } from 'react-hook-form'
import { ClusterizationAlgorithm } from '../shared/types/clsterizationAlgorith'
import { DataLinkProtocolType } from '../shared/types/dataLinkProtocolType'
import { useProjectStore } from '../store/projectStore'
import SensorProtocolSettings from './SensorProtocolSettings'

export type SensorSettingsInputs = {
    DataLinkProtocolType: DataLinkProtocolType
    ClusterizationAlgorithm: ClusterizationAlgorithm
}

export default function SensorSettings() {
    const { project, setProject } = useProjectStore()

    if (!project) {
        throw new Error('Project не определен')
    }

    const defaultValues: SensorSettingsInputs = {
        DataLinkProtocolType: project.DataLinkProtocolType,
        ClusterizationAlgorithm: project.ClusterizationAlgorithm
    }

    const { register, watch, handleSubmit, formState, reset } =
        useForm<SensorSettingsInputs>({
            defaultValues,
            reValidateMode: 'onBlur',
            mode: 'all'
        })

    // нужно, чтобы при смене проекта выставились правильные значения
    useEffect(() => {
        reset(defaultValues)
        console.log(defaultValues)
    }, [project])

    const onSubmit: SubmitHandler<SensorSettingsInputs> = async (data) => {
        const newProject = structuredClone(project)
        newProject.DataLinkProtocolType = data.DataLinkProtocolType
        newProject.ClusterizationAlgorithm = data.ClusterizationAlgorithm
        newProject.Result = undefined
        setProject(newProject)
    }

    return (
        <form onSubmit={handleSubmit(onSubmit)}>
            <Flex direction='column' gap={4}>
                <FormControl>
                    <FormLabel>Протоколы</FormLabel>
                    <SensorProtocolSettings
                        watch={watch}
                        register={register}
                        formState={formState}
                    />
                </FormControl>
                <Tooltip
                    isDisabled={project.Result === undefined}
                    label='Внимание! Данное действие удалит результаты симуляции'
                >
                    <Button isDisabled={!formState.isDirty} type='submit'>
                        <IconDeviceFloppy />
                        Сохранить изменения настройки сенсоров
                    </Button>
                </Tooltip>
            </Flex>
        </form>
    )
}
