/*
 * Athene Requirements Center
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import * as Ng2 from '@angular/core';

@Ng2.Directive({
    selector: '.gp-cloak'
})
export class CloakDirective implements Ng2.OnInit {
    constructor(private readonly element: Ng2.ElementRef) { }

    public ngOnInit() {
        this.element.nativeElement.classList.remove('gp-cloak');
    }
}