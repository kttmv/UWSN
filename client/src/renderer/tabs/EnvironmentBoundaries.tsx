import {
    Button,
    Flex,
    FormControl,
    FormErrorMessage,
    FormLabel,
    Grid,
    Input,
    Text
} from '@chakra-ui/react'
import { IconBox } from '@tabler/icons-react'
import { useEffect } from 'react'
import { SubmitHandler, useForm } from 'react-hook-form'
import { Vector3Data } from '../shared/types/vector3Data'
import useProjectStore from '../store/projectStore'

interface EnvironmentInputs {
    v1: Vector3Data
    v2: Vector3Data
}

export default function EnvironmentBoundaries() {
    const { register, handleSubmit, reset, formState } =
        useForm<EnvironmentInputs>({
            reValidateMode: 'onBlur',
            mode: 'all'
        })

    const errors = formState.errors
    const dirtyFields = formState.dirtyFields

    const { project, setProject } = useProjectStore()

    useEffect(() => {
        if (project) {
            reset({
                v1: project.AreaLimits.Min,
                v2: project.AreaLimits.Max
            })
        } else {
            reset()
        }
    }, [project])

    const onSubmit: SubmitHandler<EnvironmentInputs> = (data) => {
        const newProject = structuredClone(project)

        if (!newProject) {
            throw new Error()
        }

        newProject.AreaLimits = {
            Min: {
                X: Number(data.v1.X),
                Y: Number(data.v1.Y),
                Z: Number(data.v1.Z)
            },
            Max: {
                X: Number(data.v2.X),
                Y: Number(data.v2.Y),
                Z: Number(data.v2.Z)
            }
        }

        setProject(newProject)
        reset(data)
    }

    return (
        <form onSubmit={handleSubmit(onSubmit)}>
            <Flex direction='column' gap={4}>
                <FormControl isInvalid={Object.keys(errors).length !== 0}>
                    <FormLabel>Границы акватории</FormLabel>

                    <Grid templateColumns='repeat(3, 1fr)' gap={4}>
                        <Flex align='center' gap={2}>
                            <Text>x₁</Text>
                            <Input
                                type='number'
                                isInvalid={errors.v1?.X?.type === 'required'}
                                fontWeight={
                                    dirtyFields.v1?.X ? 'bold' : undefined
                                }
                                {...register('v1.X', {
                                    required:
                                        'Отсутствует значение параметра x₁'
                                })}
                            />
                        </Flex>

                        <Flex align='center' gap={2}>
                            <Text>y₁</Text>
                            <Input
                                type='number'
                                isInvalid={errors.v1?.Y?.type === 'required'}
                                fontWeight={
                                    dirtyFields.v1?.Y ? 'bold' : undefined
                                }
                                {...register('v1.Y', {
                                    required:
                                        'Отсутствует значение параметра y₁'
                                })}
                            />
                        </Flex>

                        <Flex align='center' gap={2}>
                            <Text>z₁</Text>
                            <Input
                                type='number'
                                isInvalid={errors.v1?.Z?.type === 'required'}
                                fontWeight={
                                    dirtyFields.v1?.Z ? 'bold' : undefined
                                }
                                {...register('v1.Z', {
                                    required:
                                        'Отсутствует значение параметра z₁'
                                })}
                            />
                        </Flex>

                        <Flex align='center' gap={2}>
                            <Text>x₂</Text>
                            <Input
                                type='number'
                                isInvalid={errors.v2?.X?.type === 'required'}
                                fontWeight={
                                    dirtyFields.v2?.X ? 'bold' : undefined
                                }
                                {...register('v2.X', {
                                    required:
                                        'Отсутствует значение параметра x₂'
                                })}
                            />
                        </Flex>

                        <Flex align='center' gap={2}>
                            <Text>y₂</Text>
                            <Input
                                type='number'
                                isInvalid={errors.v2?.Y?.type === 'required'}
                                fontWeight={
                                    dirtyFields.v2?.Y ? 'bold' : undefined
                                }
                                {...register('v2.Y', {
                                    required:
                                        'Отсутствует значение параметра y₂'
                                })}
                            />
                        </Flex>

                        <Flex align='center' gap={2}>
                            <Text>z₂</Text>
                            <Input
                                type='number'
                                isInvalid={errors.v2?.Z?.type === 'required'}
                                fontWeight={
                                    dirtyFields.v2?.Z ? 'bold' : undefined
                                }
                                {...register('v2.Z', {
                                    required:
                                        'Отсутствует значение параметра z₂'
                                })}
                            />
                        </Flex>
                    </Grid>

                    <FormErrorMessage>
                        Необходимо ввести все значения
                    </FormErrorMessage>
                </FormControl>
                <Button
                    type='submit'
                    isDisabled={!formState.isValid || !formState.isDirty}
                >
                    <IconBox />
                    <Text m={1}>Применить</Text>
                </Button>
            </Flex>
        </form>
    )
}
