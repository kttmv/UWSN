import { Button, Flex, TabPanel, Text } from '@chakra-ui/react'
import { IconBox } from '@tabler/icons-react'
import { SubmitHandler, useForm } from 'react-hook-form'
import { Vector3 } from 'three'
import { runInitialization } from '../simulator/simulatorHelper'
import EnvironmentBoundaries from './EnvironmentBoundaries'

export interface EnvironmentInputs {
    v1: Vector3
    v2: Vector3
}
export default function EnvironmentTab() {
    const {
        register,
        handleSubmit,
        watch,
        formState: { errors, isValid },
        setValue
    } = useForm<EnvironmentInputs>({
        defaultValues: {
            v1: new Vector3(),
            v2: new Vector3()
        },
        reValidateMode: 'onBlur',
        mode: 'all'
    })
    const onSubmit: SubmitHandler<EnvironmentInputs> = (data) => {
        runInitialization(data.v1, data.v2)
    }

    return (
        <TabPanel>
            <form onSubmit={handleSubmit(onSubmit)}>
                <Flex direction='column' gap={4}>
                    <EnvironmentBoundaries
                        errors={errors}
                        register={register}
                    />
                    <Button type='submit' isDisabled={!isValid}>
                        <IconBox />
                        <Text m={1}>Инициализировать окружение</Text>
                    </Button>
                </Flex>
            </form>
        </TabPanel>
    )
}
