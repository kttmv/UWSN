import { Flex, FormLabel, Grid, Input, Select, Text } from '@chakra-ui/react'
import { FormState, UseFormRegister, UseFormWatch } from 'react-hook-form'
import {
    SensorPlacementDistributionType,
    SensorPlacementInputs
} from './SensorPlacement'

type Props = {
    formState: FormState<SensorPlacementInputs>
    register: UseFormRegister<SensorPlacementInputs>
    watch: UseFormWatch<SensorPlacementInputs>
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
                    {...register('distributionType')}
                    defaultValue={SensorPlacementDistributionType.Normal}
                >
                    <option value={SensorPlacementDistributionType.Normal}>
                        Нормальное
                    </option>
                    <option value={SensorPlacementDistributionType.Uniform}>
                        Равномерное
                    </option>
                </Select>
            </Flex>

            {Number(watch('distributionType')) ===
                SensorPlacementDistributionType.Uniform && (
                <>
                    <FormLabel mt='30px'>
                        Параметры равномерного распределения
                    </FormLabel>
                    <Grid templateColumns='repeat(2, 1fr)' gap={4}>
                        <Flex align='center' gap={2}>
                            <Text>A</Text>
                            <Input
                                type='number'
                                defaultValue={0}
                                isInvalid={errors.uniformA?.type === 'required'}
                                {...register('uniformA', {
                                    required:
                                        'Отсутствует значение параметра A равномерного распределения'
                                })}
                            />
                        </Flex>

                        <Flex align='center' gap={2}>
                            <Text>B</Text>
                            <Input
                                type='number'
                                defaultValue={0}
                                isInvalid={errors.uniformB?.type === 'required'}
                                {...register('uniformB', {
                                    required:
                                        'Отсутствует значение параметра B равномерного распределения'
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
                        isInvalid={errors.countX?.type === 'required'}
                        {...register('countX', {
                            required: 'Отсутствует значение параметра X'
                        })}
                    />
                </Flex>

                <Flex align='center' gap={2}>
                    <Text>y</Text>
                    <Input
                        type='number'
                        isInvalid={errors.countY?.type === 'required'}
                        {...register('countY', {
                            required: 'Отсутствует значение параметра Y'
                        })}
                    />
                </Flex>

                <Flex align='center' gap={2}>
                    <Text>z</Text>
                    <Input
                        type='number'
                        isInvalid={errors.countZ?.type === 'required'}
                        {...register('countZ', {
                            required: 'Отсутствует значение параметра Z'
                        })}
                    />
                </Flex>
            </Grid>

            {formState.isValid && (
                <Text mt='30px'>
                    Количество сенсоров, которое будет расставлено:{' '}
                    {watch('countX') * watch('countY') * watch('countZ')}
                </Text>
            )}
        </>
    )
}
