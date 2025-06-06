import { Component } from "@angular/core";
import { Router } from "@angular/router";


@Component({
    selector: "app-not-found",
    templateUrl: "./notFound.component.html",
    styleUrls: ["./notFound.component.css"],
    standalone: false
})

export class NotFoundComponent {

    constructor(private router: Router) { }

    goToHome() {
        this.router.navigate(["/"]);
    }
}