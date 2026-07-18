import com.ghofur.services.AuthService
import kotlinx.coroutines.MainScope
import kotlinx.coroutines.launch

// ... di dalam fungsi button onClick
onClickFunction = { 
    val email = ... 
    val pass = ... 
    
    MainScope().launch {
        val success = AuthService.signIn(email, pass)
        if (success) {
            println("Login Succesfull!")
        }
    }
}

button {
    +"Sign In with Google"
    onClickFunction = { 
        MainScope().launch {
            AuthService.signInWithGoogle()
        }
    }
}