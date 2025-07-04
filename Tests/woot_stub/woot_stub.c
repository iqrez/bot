#include <stdlib.h>
static float values[256];
int wooting_analog_initialise() { return 0; }
int wooting_analog_uninitialize() { return 0; }
int wooting_analog_set_key_mode(int mode) { return 0; }
int wooting_analog_read(unsigned int device, unsigned short sc, float* value) {
    values[sc] += 0.3f;
    if(values[sc] > 1.0f) values[sc] = 0.0f;
    *value = values[sc];
    return 0;
}
