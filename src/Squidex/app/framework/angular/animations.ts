
import * as Ng2 from '@angular/core';

export const fadeAnimation = (name = 'fade', timing = '200ms'): Ng2.AnimationEntryMetadata => {
    return Ng2.trigger(
        name, [
            Ng2.transition(':enter', [
                Ng2.style({ opacity: 0 }),
                Ng2.animate(timing, Ng2.style({ opacity: 1 }))
            ]),
            Ng2.transition(':leave', [
                Ng2.style({ 'opacity': 1 }),
                Ng2.animate(timing, Ng2.style({ opacity: 0 }))
            ]),
            Ng2.state('true',
                Ng2.style({ opacity: 1 })
            ),
            Ng2.state('false',
                Ng2.style({ opacity: 0 })
            ),
            Ng2.transition('1 => 0', Ng2.animate(timing)),
            Ng2.transition('0 => 1', Ng2.animate(timing))
        ]
    );
};
    