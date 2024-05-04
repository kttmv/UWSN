import {
    Card,
    CardBody,
    CardHeader,
    Flex,
    Grid,
    Heading,
    IconButton,
    Text,
    useBreakpointValue
} from '@chakra-ui/react'
import { IconX } from '@tabler/icons-react'
import useViewerStore from '../store/viewerStore'

export default function ViewerSelectedSensorInfo() {
    const { selectedSensor, setSelectedSensor } = useViewerStore()

    const right = useBreakpointValue({
        base: '5px',
        lg: '31px'
    })

    if (!selectedSensor) {
        return <></>
    }

    const positionString =
        `{ X: ${selectedSensor.Position.X.toFixed(1)}, ` +
        `Y: ${selectedSensor.Position.Y.toFixed(1)}, ` +
        `Z: ${selectedSensor.Position.Z.toFixed(1)} }`

    let clusterString =
        selectedSensor.ClusterId !== -1
            ? selectedSensor.ClusterId.toString()
            : 'Отсутствует'

    if (selectedSensor.IsReference) {
        clusterString += ', является референсным'
    }

    return (
        <Card position='absolute' top='5px' right={right}>
            <CardHeader>
                <Flex alignItems='center'>
                    <Heading flexGrow={1} size='md'>
                        Сенсор #{selectedSensor.Id}
                    </Heading>
                    <IconButton
                        aria-label='Закрыть информацию о сенсоре'
                        onClick={() => setSelectedSensor(undefined)}
                    >
                        <IconX />
                    </IconButton>
                </Flex>
            </CardHeader>

            <CardBody>
                <Grid
                    gridTemplateColumns='min-content auto'
                    alignItems='center'
                    gap={2}
                >
                    <Text>Координаты:</Text>
                    <Card padding='5px'>{positionString}</Card>
                    <Text>Кластер:</Text>
                    <Card padding='5px'>{clusterString}</Card>
                </Grid>
            </CardBody>
        </Card>
    )
}
