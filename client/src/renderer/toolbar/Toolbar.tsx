import { Button, Card, Tooltip } from '@chakra-ui/react'
import { IconFile, IconFolderOpen } from '@tabler/icons-react'

export default function Toolbar() {
    return (
        <Card borderRadius={0} gap={1} p={1} flexDirection='row'>
            <Tooltip label='Создать новую симуляцию'>
                <Button>
                    <IconFile />
                </Button>
            </Tooltip>
            <Tooltip label='Открыть симуляцию'>
                <Button>
                    <IconFolderOpen />
                </Button>
            </Tooltip>
        </Card>
    )
}
