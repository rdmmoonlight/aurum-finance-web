plugins {
    kotlin("js") version "2.0.0"
    kotlin("plugin.serialization") version "2.0.0" // WAJIB ada agar DTO bisa diproses
}

repositories {
    mavenCentral()
}

dependencies {
    implementation("org.jetbrains.kotlinx:kotlinx-html-js:0.9.1")
    implementation(project(":packages:shared-models"))
    
    // Tambahkan Ktor Client dependencies agar ApiService bisa jalan
    implementation("io.ktor:ktor-client-core:2.3.10")
    implementation("io.ktor:ktor-client-content-negotiation:2.3.10")
    implementation("io.ktor:ktor-serialization-kotlinx-json:2.3.10")
}

kotlin {
    js(IR) {
        browser {
            binaries.executable()
        }
    }
}

// Catatan: Jika Anda sudah menggunakan import.meta.env di JS, 
// freeCompilerArgs tidak diperlukan lagi. Saya sarankan pilih salah satu.