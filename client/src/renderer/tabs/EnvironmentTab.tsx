import {
    Button,
    Flex,
    Grid,
    Heading,
    Input,
    TabPanel,
    Text
} from '@chakra-ui/react'
import { IconBox } from '@tabler/icons-react'
import { executeShellCommand } from '..'

export default function EnvironmentTab() {
    const clickedInit = () => {
        executeShellCommand(
            '..\\UWSN\\bin\\Debug\\net7.0\\UWSN.exe init -1 -1 -1 1 1 1 --file D:\\Env.json'
        )
    }
    return (
        <TabPanel>
            <Flex direction='column' gap={4}>
                <Heading size='md'>Границы:</Heading>
                <Grid
                    templateColumns={{
                        base: 'repeat(3, 1fr)',
                        lg: 'repeat(6, 1fr)'
                    }}
                    gap={4}
                >
                    <Flex align='center' gap={2}>
                        <Text>x₁</Text>
                        <Input />
                    </Flex>
                    <Flex align='center' gap={2}>
                        <Text>y₁</Text>
                        <Input />
                    </Flex>
                    <Flex align='center' gap={2}>
                        <Text>z₁</Text>
                        <Input />
                    </Flex>
                    <Flex align='center' gap={2}>
                        <Text>x₂</Text>
                        <Input />
                    </Flex>
                    <Flex align='center' gap={2}>
                        <Text>y₂</Text>
                        <Input />
                    </Flex>
                    <Flex align='center' gap={2}>
                        <Text>z₂</Text>
                        <Input />
                    </Flex>
                </Grid>
                <Button onClick={clickedInit}>
                    <IconBox />
                    <Text m={1}>Инициализировать окружение</Text>
                </Button>
            </Flex>
        </TabPanel>
    )
}
