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
import useViewerStore from '../store/viewerStore'
import SensorPlacementRandomStep, {
    RandomStepParameters
} from './SensorPlacementRandomStep'

export enum SensorPlacementType {
    None = -1,
    RandomStep
}

export enum RandomStepDistributionType {
    Normal,
    Uniform
}

export type SensorPlacementInputs = {
    selectedType: SensorPlacementType
    randomStepParameters: RandomStepParameters | undefined
}

export default function SensorPlacement() {
    const { projectFilePath, project, setProject, updateProject } =
        useProjectStore()

    const { setIsOpen: setConsoleIsOpen } = useConsoleStore()

    const { setIsOpen: setViewerIsOpen } = useViewerStore()

    const defaultValues: SensorPlacementInputs = {
        selectedType: SensorPlacementType.None,
        randomStepParameters: undefined
    }

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
                if (!data.randomStepParameters) {
                    throw new Error('Не определены параметры распределения')
                }

                await runPlaceSensorsRandomStep(
                    data.randomStepParameters.distributionType,
                    data.randomStepParameters.countX,
                    data.randomStepParameters.countY,
                    data.randomStepParameters.countZ,
                    data.randomStepParameters.uniformA,
                    data.randomStepParameters.uniformB,
                    projectFilePath
                )
                break
            }
            default: {
                throw new Error('Незвестный тип расстановки сенсоров')
            }
        }

        updateProject()
        setViewerIsOpen(true)
    }

    const selectedType = Number(watch('selectedType'))

    return (
        <form onSubmit={handleSubmit(onSubmit)}>
            <FormControl display='flex' flexDirection='column' gap={4}>
                <FormLabel>Расстановка сенсоров</FormLabel>
                <Flex align='center' gap={2}>
                    <Text minWidth='150px'>Тип расстановки</Text>
                    <Select
                        {...register('selectedType')}
                        defaultValue={SensorPlacementType.None}
                    >
                        <option value={SensorPlacementType.None}>
                            Выберите тип расстановки
                        </option>

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
                        formState.isValid &&
                        selectedType !== SensorPlacementType.None
                            ? 'Внимание! Данное действие удалит все ' +
                              'текущие сенсоры и результаты симуляции'
                            : 'Выбраны неверные параметры расстановки'
                    }
                >
                    <Button
                        width='100%'
                        type='submit'
                        isDisabled={
                            !formState.isValid ||
                            selectedType === SensorPlacementType.None
                        }
                    >
                        <IconVector />
                        <Text m={1}>Расставить сенсоры по акватории</Text>
                    </Button>
                </Tooltip>
            </FormControl>
        </form>
    )
}
