package detectors;

import java.util.HashMap;
import java.util.Map;

import edu.memphis.ccrg.lida.pam.tasks.BasicDetectionAlgorithm;
import ws3dproxy.model.Thing;

public class JewelDetector extends BasicDetectionAlgorithm {

    private final String modality = "";
    private Map<String, Object> detectorParams = new HashMap<>();

    @Override
    public void init() {
        super.init();
        detectorParams.put("mode", "leafletJewel");
    }

    @Override
    public double detect() {
        Thing leafletJewel = (Thing) sensoryMemory.getSensoryContent(modality, detectorParams);
        double activation = 0.0;
        if (leafletJewel == null) {
            activation = 1.0;
        }
        return activation;
    }
}
