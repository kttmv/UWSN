import {
    Button,
    Checkbox,
    Flex,
    FormLabel,
    Grid,
    GridItem,
    Input,
    Text,
    Tooltip
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
                <GridItem colSpan={2}>
                    <FormLabel>Параметры симуляции</FormLabel>
                </GridItem>

                <Checkbox
                    {...form.register('ShouldSkipHello')}
                    fontWeight={
                        form.formState.dirtyFields.ShouldSkipHello
                            ? 'bold'
                            : undefined
                    }
                    whiteSpace='nowrap'
                >
                    <Tooltip
                        label={
                            'При включенном значении каждый сенсор будет обладать полной ' +
                            'информацией обо всех сенсорах в сети с самого начала симуляции.'
                        }
                    >
                        Пропускать процесс HELLO
                    </Tooltip>
                </Checkbox>

                <Checkbox
                    {...form.register('ShouldSkipCycles')}
                    fontWeight={
                        form.formState.dirtyFields.ShouldSkipCycles
                            ? 'bold'
                            : undefined
                    }
                    whiteSpace='nowrap'
                >
                    <Tooltip
                        label={
                            'Полностью обрабатывает заданное количество циклов, ' +
                            'после чего отнимает средний расход батареи за цикл у ' +
                            'каждого сенсора, пока один из них не разрядится. Затем ' +
                            'процесс повторяется.'
                        }
                    >
                        Пропускать циклы
                    </Tooltip>
                </Checkbox>

                <Flex align='center' gap={2}>
                    <Tooltip
                        label={
                            'Количество циклов с начала симуляции или после разрядки сенсора, ' +
                            'которое будет обработано вручную, ' +
                            'прежде чем начнется пропуск циклов.'
                        }
                    >
                        <Text
                            whiteSpace='nowrap'
                            display={
                                !form.watch().ShouldSkipCycles
                                    ? 'none'
                                    : undefined
                            }
                        >
                            Циклов до пропуска:
                        </Text>
                    </Tooltip>

                    <Input
                        isInvalid={
                            form.formState.errors.CyclesCountBeforeSkip !==
                            undefined
                        }
                        type='number'
                        {...form.register('CyclesCountBeforeSkip', {
                            required: true
                        })}
                        fontWeight={
                            form.formState.dirtyFields.CyclesCountBeforeSkip
                                ? 'bold'
                                : undefined
                        }
                        display={
                            !form.watch().ShouldSkipCycles ? 'none' : undefined
                        }
                    />
                </Flex>

                <FormLabel marginTop={10}>Условия остановки</FormLabel>
                <Grid
                    alignItems='center'
                    templateColumns='min-content auto'
                    gap={4}
                >
                    <Text whiteSpace='nowrap'>
                        Максимальное количество событий:
                    </Text>
                    <Input
                        isInvalid={
                            form.formState.errors.MaxProcessedEvents !==
                            undefined
                        }
                        type='number'
                        {...form.register('MaxProcessedEvents', {
                            required: true
                        })}
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
                        isInvalid={
                            form.formState.errors.MaxCycles !== undefined
                        }
                        type='number'
                        {...form.register('MaxCycles', {
                            required: true
                        })}
                        fontWeight={
                            form.formState.dirtyFields.MaxCycles
                                ? 'bold'
                                : undefined
                        }
                    />

                    <Text whiteSpace='nowrap'>
                        Мертвых сенсоров для остановки (%):
                    </Text>
                    <Input
                        isInvalid={
                            form.formState.errors.DeadSensorsPercent !==
                            undefined
                        }
                        type='number'
                        {...form.register('DeadSensorsPercent', {
                            required: true
                        })}
                        fontWeight={
                            form.formState.dirtyFields.DeadSensorsPercent
                                ? 'bold'
                                : undefined
                        }
                    />
                </Grid>

                <GridItem colSpan={2}>
                    <FormLabel marginTop={10}>Вывод и результаты</FormLabel>
                </GridItem>

                <Flex align='center' gap={2}>
                    <Text
                        whiteSpace='nowrap'
                        display={
                            !form.watch().ShouldSkipHello ? 'none' : undefined
                        }
                    >
                        Выводить состояние во время
                        <br /> HELLO каждые N событий:
                    </Text>
                    <Input
                        isInvalid={
                            form.formState.errors.PrintEveryNthEvent !==
                            undefined
                        }
                        marginLeft={2}
                        type='number'
                        {...form.register('PrintEveryNthEvent', {
                            required: true
                        })}
                        fontWeight={
                            form.formState.dirtyFields.PrintEveryNthEvent
                                ? 'bold'
                                : undefined
                        }
                        display={
                            !form.watch().ShouldSkipHello ? 'none' : undefined
                        }
                    />
                </Flex>

                <Checkbox
                    {...form.register('Verbose')}
                    fontWeight={
                        form.formState.dirtyFields.Verbose ? 'bold' : undefined
                    }
                    whiteSpace='nowrap'
                >
                    <Tooltip
                        label={
                            'Подробный вывод симуляции. Значительно замедляет симуляцию и может приводить к ' +
                            'очень большому расходу оперативной памяти.'
                        }
                    >
                        Подробный вывод
                    </Tooltip>
                </Checkbox>

                <Checkbox
                    {...form.register('SaveOutput')}
                    fontWeight={
                        form.formState.dirtyFields.SaveOutput
                            ? 'bold'
                            : undefined
                    }
                    whiteSpace='nowrap'
                >
                    <Tooltip
                        label={
                            'Сохранение всего вывода консоли в файл output.txt.'
                        }
                    >
                        Сохранение вывода консоли в файл
                    </Tooltip>
                </Checkbox>

                <Checkbox
                    {...form.register('CreateAllDeltas')}
                    fontWeight={
                        form.formState.dirtyFields.CreateAllDeltas
                            ? 'bold'
                            : undefined
                    }
                    whiteSpace='nowrap'
                >
                    <Tooltip
                        label={
                            'Сохраняет результат каждой пересылки между сенсорами. ' +
                            'Приводит к очень большому размеру файла симуляции ' +
                            '(Гигабайты для продолжительных симуляци).'
                        }
                    >
                        Сохранение сигналов
                    </Tooltip>
                </Checkbox>

                {form.formState.isDirty &&
                    form.watch().SaveOutput &&
                    form.watch().Verbose && (
                        <Text color='red' fontWeight='bold'>
                            Внимание! При одновременном выборе опций "Подробный
                            вывод" и "Сохранение вывода консоли в файл" размер
                            сохраненного файла может составлять гигабайты.
                            Использовать с осторожностью.
                        </Text>
                    )}

                <Button
                    marginTop={6}
                    width='100%'
                    isDisabled={
                        !form.formState.isDirty || !form.formState.isValid
                    }
                    type='submit'
                >
                    <IconDeviceFloppy />{' '}
                    <Text marginLeft={1}>
                        Сохранить изменения настройки симуляции
                    </Text>
                </Button>
            </Flex>
        </form>
    )
}
