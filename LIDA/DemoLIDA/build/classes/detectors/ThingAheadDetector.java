package detectors;

import java.util.HashMap;
import java.util.Map;

import edu.memphis.ccrg.lida.pam.tasks.BasicDetectionAlgorithm;
import java.util.List;
import ws3dproxy.model.Thing;

public class ThingAheadDetector extends BasicDetectionAlgorithm {

    private final String modality = "";
    private Map<String, Object> detectorParams = new HashMap<>();

    @Override
    public void init() {
        super.init();
        detectorParams.put("mode", "thingAhead");
    }

    @Override
    public double detect() {
        List<Thing> thingAhead = (List<Thing>) sensoryMemory.getSensoryContent(modality, detectorParams);
        double activation = 0.0;
        if (!thingAhead.isEmpty()) {
            activation = 1.0;
        }
        return activation;
    }
}
