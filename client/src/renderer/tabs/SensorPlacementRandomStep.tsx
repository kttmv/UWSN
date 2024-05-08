import { Flex, FormLabel, Grid, Input, Select, Text } from '@chakra-ui/react'
import { FormState, UseFormRegister, UseFormWatch } from 'react-hook-form'
import {
    RandomStepDistributionType,
    SensorPlacementInputs
} from './SensorPlacement'

type Props = {
    formState: FormState<SensorPlacementInputs>
    register: UseFormRegister<SensorPlacementInputs>
    watch: UseFormWatch<SensorPlacementInputs>
}

export type RandomStepParameters = {
    distributionType: RandomStepDistributionType
    uniformA: number
    uniformB: number
    countX: number
    countY: number
    countZ: number
}

export default function SensorPlacementRandomStep({
    formState,
    register,
    watch
}: Props) {
    const errors = formState.errors

    return (
        <>
            <Flex align='center' gap={2}>
                <Text minWidth='150px'>Тип распределения</Text>
                <Select
                    title='Выбор типа распределения'
                    {...register('randomStepParameters.distributionType')}
                    defaultValue={RandomStepDistributionType.Normal}
                >
                    <option value={RandomStepDistributionType.Normal}>
                        Нормальное
                    </option>
                    <option value={RandomStepDistributionType.Uniform}>
                        Равномерное
                    </option>
                </Select>
            </Flex>

            {Number(watch('randomStepParameters.distributionType')) ===
                RandomStepDistributionType.Uniform && (
                <>
                    <FormLabel mt='30px'>
                        Параметры равномерного распределения
                    </FormLabel>
                    <Grid templateColumns='repeat(2, 1fr)' gap={4}>
                        <Flex align='center' gap={2}>
                            <Text>A</Text>
                            <Input
                                type='number'
                                isInvalid={
                                    errors.randomStepParameters?.uniformA
                                        ?.type === 'required'
                                }
                                {...register('randomStepParameters.uniformA', {
                                    required:
                                        'Отсутствует значение параметра A ' +
                                        'равномерного распределения'
                                })}
                            />
                        </Flex>

                        <Flex align='center' gap={2}>
                            <Text>B</Text>
                            <Input
                                type='number'
                                isInvalid={
                                    errors.randomStepParameters?.uniformB
                                        ?.type === 'required'
                                }
                                {...register('randomStepParameters.uniformB', {
                                    required:
                                        'Отсутствует значение параметра B ' +
                                        'равномерного распределения'
                                })}
                            />
                        </Flex>
                    </Grid>
                </>
            )}

            <FormLabel mt='30px'>Количество сенсоров по ...</FormLabel>
            <Grid templateColumns='repeat(3, 1fr)' gap={4}>
                <Flex align='center' gap={2}>
                    <Text>x</Text>
                    <Input
                        type='number'
                        isInvalid={
                            errors.randomStepParameters?.countX?.type ===
                            'required'
                        }
                        {...register('randomStepParameters.countX', {
                            required: 'Отсутствует значение параметра X'
                        })}
                    />
                </Flex>

                <Flex align='center' gap={2}>
                    <Text>y</Text>
                    <Input
                        type='number'
                        isInvalid={
                            errors.randomStepParameters?.countY?.type ===
                            'required'
                        }
                        {...register('randomStepParameters.countY', {
                            required: 'Отсутствует значение параметра Y'
                        })}
                    />
                </Flex>

                <Flex align='center' gap={2}>
                    <Text>z</Text>
                    <Input
                        type='number'
                        isInvalid={
                            errors.randomStepParameters?.countZ?.type ===
                            'required'
                        }
                        {...register('randomStepParameters.countZ', {
                            required: 'Отсутствует значение параметра Z'
                        })}
                    />
                </Flex>
            </Grid>

            {formState.isValid && (
                <Text mt='30px'>
                    Количество сенсоров, которое будет расставлено:{' '}
                    {watch('randomStepParameters.countX') *
                        watch('randomStepParameters.countY') *
                        watch('randomStepParameters.countZ')}
                </Text>
            )}
        </>
    )
}
