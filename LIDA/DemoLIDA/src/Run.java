

import edu.memphis.ccrg.lida.framework.initialization.AgentStarter;
import java.util.logging.Level;
import java.util.logging.Logger;

public class Run {
    
    public static void main(String[] args ){
        Logger.getLogger("edu.memphis.ccrg.lida").setLevel(Level.WARNING);
        AgentStarter.main(args);
    }
    
}
