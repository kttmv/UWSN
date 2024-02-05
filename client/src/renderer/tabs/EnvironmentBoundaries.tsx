import {
    Flex,
    FormControl,
    FormErrorMessage,
    FormLabel,
    Grid,
    Input,
    Text
} from '@chakra-ui/react'
import { FieldErrors, UseFormRegister } from 'react-hook-form'
import { EnvironmentInputs } from './EnvironmentTab'

interface Props {
    register: UseFormRegister<EnvironmentInputs>
    errors: FieldErrors<EnvironmentInputs>
}

export default function EnvironmentBoundaries({ register, errors }: Props) {
    return (
        <>
            <FormControl isInvalid={Object.keys(errors).length !== 0}>
                <FormLabel>Границы</FormLabel>
                <Grid templateColumns='repeat(3, 1fr)' gap={4}>
                    <Flex align='center' gap={2}>
                        <Text>x₁</Text>
                        <Input
                            type='number'
                            isInvalid={errors.v1?.x?.type === 'required'}
                            {...register('v1.x', {
                                required: 'Отсутствует значение параметра x₁'
                            })}
                        />
                    </Flex>
                    <Flex align='center' gap={2}>
                        <Text>y₁</Text>
                        <Input
                            type='number'
                            isInvalid={errors.v1?.y?.type === 'required'}
                            {...register('v1.y', {
                                required: 'Отсутствует значение параметра y₁'
                            })}
                        />
                    </Flex>
                    <Flex align='center' gap={2}>
                        <Text>z₁</Text>
                        <Input
                            type='number'
                            isInvalid={errors.v1?.z?.type === 'required'}
                            {...register('v1.z', {
                                required: 'Отсутствует значение параметра z₁'
                            })}
                        />
                    </Flex>
                    <Flex align='center' gap={2}>
                        <Text>x₂</Text>
                        <Input
                            type='number'
                            isInvalid={errors.v2?.x?.type === 'required'}
                            {...register('v2.x', {
                                required: 'Отсутствует значение параметра x₂'
                            })}
                        />
                    </Flex>
                    <Flex align='center' gap={2}>
                        <Text>y₂</Text>
                        <Input
                            type='number'
                            isInvalid={errors.v2?.y?.type === 'required'}
                            {...register('v2.y', {
                                required: 'Отсутствует значение параметра y₂'
                            })}
                        />
                    </Flex>
                    <Flex align='center' gap={2}>
                        <Text>z₂</Text>
                        <Input
                            type='number'
                            isInvalid={errors.v2?.z?.type === 'required'}
                            {...register('v2.z', {
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
