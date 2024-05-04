import { Card, CardBody, CardHeader } from '@chakra-ui/react'
import useViewerStore from '../store/viewerStore'

export default function ViewerSelectedSensorInfo() {
    const { selectedSensor, setSelectedSensor } = useViewerStore()

    if (!selectedSensor) {
        return <></>
    }

    return (
        <Card position='absolute' top='5px' right='7px'>
            <CardHeader>Сенсор #{selectedSensor.Id}</CardHeader>

            <CardBody></CardBody>
        </Card>
    )
}
