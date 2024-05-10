import {
    Button,
    Checkbox,
    Flex,
    FormLabel,
    Grid,
    GridItem,
    Input,
    Text
} from '@chakra-ui/react'
import { IconDeviceFloppy } from '@tabler/icons-react'
import { useEffect } from 'react'
import { SubmitHandler, useForm } from 'react-hook-form'
import { SimulationSettings } from '../shared/types/simulationSettings'
import { useProjectStore } from '../store/projectStore'

export default function SimulationSettings() {
    const { project, setProject } = useProjectStore()

    if (!project) {
        throw new Error('Project не определен')
    }

    const defaultValues: SimulationSettings = structuredClone(
        project.SimulationSettings
    )

    const form = useForm<SimulationSettings>({
        defaultValues,
        reValidateMode: 'onBlur',
        mode: 'all'
    })

    // нужно, чтобы при смене проекта выставились правильные значения
    useEffect(() => {
        form.reset(defaultValues)
        console.log(defaultValues)
    }, [project])

    const onSubmit: SubmitHandler<SimulationSettings> = async (data) => {
        const newProject = structuredClone(project)
        newProject.SimulationSettings = data
        newProject.Result = undefined
        setProject(newProject)
    }

    return (
        <form onSubmit={form.handleSubmit(onSubmit)}>
            <Flex direction='column' gap={4}>
                <FormLabel marginTop={10}>Параметры симуляции</FormLabel>
                <Grid
                    alignItems='center'
                    templateColumns='min-content auto'
                    gap={4}
                >
                    <Text whiteSpace='nowrap'>
                        Максимальное количество событий:
                    </Text>
                    <Input
                        type='number'
                        {...form.register('MaxProcessedEvents')}
                        fontWeight={
                            form.formState.dirtyFields.MaxProcessedEvents
                                ? 'bold'
                                : undefined
                        }
                    />

                    <Text whiteSpace='nowrap'>
                        Максимальное количество циклов:
                    </Text>
                    <Input
                        type='number'
                        {...form.register('MaxCycles')}
                        fontWeight={
                            form.formState.dirtyFields.MaxCycles
                                ? 'bold'
                                : undefined
                        }
                    />

                    <Text whiteSpace='nowrap'>
                        Печатать состояние каждые N событий:
                    </Text>
                    <Input
                        type='number'
                        {...form.register('PrintEveryNthEvent')}
                        fontWeight={
                            form.formState.dirtyFields.PrintEveryNthEvent
                                ? 'bold'
                                : undefined
                        }
                    />

                    <Text whiteSpace='nowrap'>
                        Мертвых сенсоров для остановки (%):
                    </Text>
                    <Input
                        type='number'
                        {...form.register('DeadSensorsPercent')}
                        fontWeight={
                            form.formState.dirtyFields.DeadSensorsPercent
                                ? 'bold'
                                : undefined
                        }
                    />

                    <GridItem colSpan={2}>
                        <Checkbox
                            {...form.register('ShouldSkipHello')}
                            fontWeight={
                                form.formState.dirtyFields.ShouldSkipHello
                                    ? 'bold'
                                    : undefined
                            }
                        >
                            Пропускать процесс HELLO
                        </Checkbox>
                    </GridItem>
                </Grid>

                <Button
                    marginTop={6}
                    width='100%'
                    isDisabled={!form.formState.isDirty}
                    type='submit'
                >
                    <IconDeviceFloppy />
                    Сохранить изменения настройки симуляции
                </Button>
            </Flex>
        </form>
    )
}
