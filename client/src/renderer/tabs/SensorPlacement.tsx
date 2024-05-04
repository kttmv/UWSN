import {
    Button,
    Flex,
    FormControl,
    FormLabel,
    Select,
    Text,
    Tooltip
} from '@chakra-ui/react'
import { IconVector } from '@tabler/icons-react'
import { SubmitHandler, useForm } from 'react-hook-form'
import { runPlaceSensorsRandomStep } from '../simulator/simulatorHelper'
import useConsoleStore from '../store/consoleStore'
import { useProjectStore } from '../store/projectStore'
import SensorPlacementRandomStep from './SensorPlacementRandomStep'

export enum SensorPlacementType {
    RandomStep = 1
}

export enum SensorPlacementDistributionType {
    Normal = 1,
    Uniform
}

export type SensorPlacementInputs = {
    selectedType: SensorPlacementType
    distributionType: SensorPlacementDistributionType
    uniformA: number
    uniformB: number
    countX: number
    countY: number
    countZ: number
}

export default function SensorPlacement() {
    const { projectFilePath, project, setProject, updateProject } =
        useProjectStore()
    const { setIsOpen: setConsoleIsOpen } = useConsoleStore()

    const { register, handleSubmit, watch, formState } =
        useForm<SensorPlacementInputs>({
            reValidateMode: 'onBlur',
            mode: 'all'
        })

    const onSubmit: SubmitHandler<SensorPlacementInputs> = async (data) => {
        if (!project) {
            throw new Error('Project не может быть undefined')
        }
        const newProject = structuredClone(project)
        newProject.Result = undefined
        setProject(newProject)

        setConsoleIsOpen(true)

        const type = Number(data.selectedType)

        switch (type) {
            case SensorPlacementType.RandomStep: {
                await runPlaceSensorsRandomStep(
                    data.distributionType,
                    data.countX,
                    data.countY,
                    data.countZ,
                    data.uniformA,
                    data.uniformB,
                    projectFilePath
                )
                break
            }
            default: {
                throw new Error('Незвестный тип расстановки сенсоров')
            }
        }

        updateProject()
    }

    const selectedType = Number(watch('selectedType'))

    return (
        <form onSubmit={handleSubmit(onSubmit)}>
            <FormControl display='flex' flexDirection='column' gap={4}>
                <FormLabel>Расстановка сенсоров</FormLabel>
                <Flex align='center' gap={2}>
                    <Text minWidth='150px'>Тип расстановки</Text>
                    <Select {...register('selectedType')} defaultValue={-1}>
                        <option value={-1}>Выберите тип расстановки</option>
                        <option value={SensorPlacementType.RandomStep}>
                            Ортогональное распределение со случайным шагом
                        </option>
                    </Select>
                </Flex>

                {selectedType === SensorPlacementType.RandomStep && (
                    <SensorPlacementRandomStep
                        watch={watch}
                        formState={formState}
                        register={register}
                    />
                )}

                <Tooltip
                    label={
                        'Внимание! Данное действие удалит все ' +
                        'текущие сенсоры и результаты симуляции'
                    }
                >
                    <Button
                        width='100%'
                        type='submit'
                        isDisabled={!formState.isValid}
                    >
                        <IconVector />
                        <Text m={1}>Расставить сенсоры по акватории</Text>
                    </Button>
                </Tooltip>
            </FormControl>
        </form>
    )
}
