import { Button, Flex, TabPanel, Text } from '@chakra-ui/react'
import { IconBox } from '@tabler/icons-react'
import { SubmitHandler, useForm } from 'react-hook-form'
import { Vector3Data } from '../shared/types/vector3Data'
import useProjectStore from '../store/projectStore'
import EnvironmentBoundaries from './EnvironmentBoundaries'

export interface EnvironmentInputs {
    v1: Vector3Data
    v2: Vector3Data
}

export default function EnvironmentTab() {
    const { register, handleSubmit, reset, formState } =
        useForm<EnvironmentInputs>({
            defaultValues: {
                v1: { X: 0, Y: 0, Z: 0 },
                v2: { X: 0, Y: 0, Z: 0 }
            },
            reValidateMode: 'onBlur',
            mode: 'all'
        })

    const { project, setProject } = useProjectStore()

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
        <TabPanel>
            <form onSubmit={handleSubmit(onSubmit)}>
                <Flex direction='column' gap={4}>
                    <EnvironmentBoundaries
                        register={register}
                        formState={formState}
                    />
                    <Button
                        type='submit'
                        isDisabled={!formState.isValid || !formState.isDirty}
                    >
                        <IconBox />
                        <Text m={1}>Применить</Text>
                    </Button>
                </Flex>
            </form>
        </TabPanel>
    )
}
