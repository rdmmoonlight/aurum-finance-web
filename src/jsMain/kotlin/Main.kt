import androidx.compose.runtime.* // Jika pakai Compose, atau gunakan Ktor Coroutine
import com.aurum.api.ApiService
import kotlinx.browser.document
import kotlinx.coroutines.*
import kotlinx.html.*
import kotlinx.html.dom.append

fun main() {
    val apiService = ApiService()
    
    // Gunakan MainScope untuk menjalankan coroutine di JS
    MainScope().launch {
        try {
            val transactions = apiService.fetchTransactions()
            renderTransactions(transactions)
        } catch (e: Exception) {
            println("Error fetching data: ${e.message}")
        }
    }
}

fun renderTransactions(transactions: List<com.aurum.models.Transaction>) {
    document.body?.append?.div {
        h1 { +"Transaction Ledger" }
        table {
            thead {
                tr { th { +"ID" }; th { +"Amount" }; th { +"Category" } }
            }
            tbody {
                transactions.forEach { t ->
                    tr {
                        td { +t.id }
                        td { +t.amount.toString() }
                        td { +t.category }
                    }
                }
            }
        }
    }
}