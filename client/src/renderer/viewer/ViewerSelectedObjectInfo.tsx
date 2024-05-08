import {
    Card,
    CardBody,
    CardHeader,
    Flex,
    Grid,
    Heading,
    IconButton,
    useBreakpointValue
} from '@chakra-ui/react'
import { IconX } from '@tabler/icons-react'
import { useProjectStore } from '../store/projectStore'
import useViewerStore from '../store/viewerStore'
import SensorInfo from './SensorInfo'
import SignalInfo from './SignalInfo'

export default function ViewerSelectedObjectInfo() {
    const {
        selectedSensor,
        setSelectedSensor,
        selectedSignal,
        setSelectedSignal
    } = useViewerStore()

    const right = useBreakpointValue({
        base: '5px',
        lg: '31px'
    })

    const { project } = useProjectStore()
    if (!project) {
        throw new Error('Project не определен')
    }

    if (!selectedSensor && !selectedSignal) {
        return <></>
    }

    const onCloseClick = () => {
        setSelectedSensor(undefined)
        setSelectedSignal(undefined)
    }

    return (
        <Card
            position='absolute'
            top='5px'
            right={right}
            overflowY='auto'
            maxHeight={{ base: '20vh', lg: '50vh' }}
        >
            <CardHeader>
                <Flex alignItems='center'>
                    <Heading flexGrow={1} size='md'>
                        {selectedSensor && `Сенсор #${selectedSensor.Id}`}

                        {selectedSignal &&
                            `Отправка кадра #${selectedSignal.FrameId}`}
                    </Heading>
                    <IconButton
                        aria-label='Закрыть информацию об объекте'
                        onClick={onCloseClick}
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
                    {selectedSensor && <SensorInfo sensor={selectedSensor} />}
                    {selectedSignal && <SignalInfo signal={selectedSignal} />}
                </Grid>
            </CardBody>
        </Card>
    )
}
