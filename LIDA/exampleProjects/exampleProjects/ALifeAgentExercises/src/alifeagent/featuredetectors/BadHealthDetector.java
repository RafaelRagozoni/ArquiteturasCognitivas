package alifeagent.featuredetectors;

/*
 * Click nbfs://nbhost/SystemFileSystem/Templates/Licenses/license-default.txt to change this license
 * Click nbfs://nbhost/SystemFileSystem/Templates/Classes/Class.java to edit this template
 */

import edu.memphis.ccrg.lida.pam.tasks.BasicDetectionAlgorithm;
import java.util.HashMap;
import java.util.Map;

/**
 *
 * @author rafael
 */
public class BadHealthDetector extends BasicDetectionAlgorithm {
    private final String modality = "";
    private Map<String, Object> detectorParams = new HashMap<String, Object>();
    
    @Override
    public void init() {
        super.init();
        detectorParams.put("mode", "health");
    }
    
    @Override
    public double detect() {
        double healthValue = (Double) sensoryMemory.getSensoryContent(modality, detectorParams);
        double activation = 0.0;
        if (healthValue <= 0.33) {
            activation = 1.0;
        }
        return activation;
    }
    
}
