package detectors;

import java.util.HashMap;
import java.util.Map;

import edu.memphis.ccrg.lida.pam.tasks.BasicDetectionAlgorithm;
import ws3dproxy.model.Thing;

public class FoodDetector extends BasicDetectionAlgorithm {

    private final String modality = "";
    private Map<String, Object> detectorParams = new HashMap<>();

    @Override
    public void init() {
        super.init();
        detectorParams.put("mode", "food");
    }

    @Override
    public double detect() {
        Thing food = (Thing) sensoryMemory.getSensoryContent(modality, detectorParams);
        double activation = 0.0;
        if (food != null) {
            activation = 1.0;
        }
        return activation;
    }
}
