import {
    Flex,
    FormControl,
    FormErrorMessage,
    FormLabel,
    Grid,
    Input,
    Text
} from '@chakra-ui/react'
import { FormState, UseFormRegister } from 'react-hook-form'
import { EnvironmentInputs } from './EnvironmentTab'

interface Props {
    register: UseFormRegister<EnvironmentInputs>
    formState: FormState<EnvironmentInputs>
}

export default function EnvironmentBoundaries({ register, formState }: Props) {
    const errors = formState.errors
    const dirtyFields = formState.dirtyFields

    return (
        <>
            <FormControl isInvalid={Object.keys(errors).length !== 0}>
                <FormLabel>Границы</FormLabel>

                <Grid templateColumns='repeat(3, 1fr)' gap={4}>
                    <Flex align='center' gap={2}>
                        <Text>x₁</Text>
                        <Input
                            type='number'
                            isInvalid={errors.v1?.X?.type === 'required'}
                            fontWeight={dirtyFields.v1?.X ? 'bold' : undefined}
                            {...register('v1.X', {
                                required: 'Отсутствует значение параметра x₁'
                            })}
                        />
                    </Flex>

                    <Flex align='center' gap={2}>
                        <Text>y₁</Text>
                        <Input
                            type='number'
                            isInvalid={errors.v1?.Y?.type === 'required'}
                            fontWeight={dirtyFields.v1?.Y ? 'bold' : undefined}
                            {...register('v1.Y', {
                                required: 'Отсутствует значение параметра y₁'
                            })}
                        />
                    </Flex>

                    <Flex align='center' gap={2}>
                        <Text>z₁</Text>
                        <Input
                            type='number'
                            isInvalid={errors.v1?.Z?.type === 'required'}
                            fontWeight={dirtyFields.v1?.Z ? 'bold' : undefined}
                            {...register('v1.Z', {
                                required: 'Отсутствует значение параметра z₁'
                            })}
                        />
                    </Flex>

                    <Flex align='center' gap={2}>
                        <Text>x₂</Text>
                        <Input
                            type='number'
                            isInvalid={errors.v2?.X?.type === 'required'}
                            fontWeight={dirtyFields.v2?.X ? 'bold' : undefined}
                            {...register('v2.X', {
                                required: 'Отсутствует значение параметра x₂'
                            })}
                        />
                    </Flex>

                    <Flex align='center' gap={2}>
                        <Text>y₂</Text>
                        <Input
                            type='number'
                            isInvalid={errors.v2?.Y?.type === 'required'}
                            fontWeight={dirtyFields.v2?.Y ? 'bold' : undefined}
                            {...register('v2.Y', {
                                required: 'Отсутствует значение параметра y₂'
                            })}
                        />
                    </Flex>

                    <Flex align='center' gap={2}>
                        <Text>z₂</Text>
                        <Input
                            type='number'
                            isInvalid={errors.v2?.Z?.type === 'required'}
                            fontWeight={dirtyFields.v2?.Z ? 'bold' : undefined}
                            {...register('v2.Z', {
                                required: 'Отсутствует значение параметра z₂'
                            })}
                        />
                    </Flex>
                </Grid>

                <FormErrorMessage>
                    Необходимо ввести все значения
                </FormErrorMessage>
            </FormControl>
        </>
    )
}
