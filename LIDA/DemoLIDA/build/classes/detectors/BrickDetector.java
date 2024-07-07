package detectors;

import java.util.HashMap;
import java.util.Map;

import edu.memphis.ccrg.lida.pam.tasks.BasicDetectionAlgorithm;
import java.awt.geom.Rectangle2D;
import java.util.List;
import modules.Environment;
import ws3dproxy.model.Thing;
import ws3dproxy.model.WorldPoint;

public class BrickDetector extends BasicDetectionAlgorithm {

    private final String modality = "";
    private Map<String, Object> detectorParams = new HashMap<>();

    @Override
    public void init() {
        super.init();
        detectorParams.put("mode", "brick");
    }

    @Override
    public double detect() {
        Thing brick = (Thing) sensoryMemory.getSensoryContent(modality, detectorParams);
        double activation = 0.0;
        if (brick != null) {
              activation = 1.0;
        }
        return activation;
    }
}
